import { InteropStream } from 'stream.js';
import { sendMessage, sendMessageSync } from 'messages.js';
import { PlanckMessage } from '@planck/types';
import { waitFor } from 'utils.js';

const moduleMap = Object.create(null) as Record<
  string,
  Awaited<ReturnType<typeof createProxy>>
>;
const modulesLoading: string[] = [];

type ModulePropChangedBody = {
  Name: string;
  Value: any;
  Module: string;
};

window.chrome.webview.addEventListener<PlanckMessage<ModulePropChangedBody>>(
  'message',
  (args) => {
    if (args.data?.command === 'MODULE_PROP_CHANGED') {
      const { Name, Value, Module } = args.data.body!;
      console.debug('Received module prop updated', Name, Module);
      const savedModule = moduleMap[Module];
      if (typeof savedModule !== 'undefined') {
        savedModule.updateValue(Name, Value);
      }
    }
  }
);

export async function _import<TModule>(id: string) {
  if (modulesLoading.includes(id)) {
    try {
      console.debug('Loading in process, waiting', id);
      await waitFor(() => !modulesLoading.includes(id));
      console.debug('Completed wait for', id);
      return moduleMap[id] as TModule;
    } catch (ex) {
      // ignore
      console.debug(ex);
    }
  }
  if (id in moduleMap) {
    return moduleMap[id] as TModule;
  }
  modulesLoading.push(id);
  const moduleConfiguration = await sendMessage<ExportDefinition[]>(
    'LOAD_MODULE',
    { id }
  );
  const proxy = await createProxy(id, moduleConfiguration);
  modulesLoading.splice(modulesLoading.indexOf(id), 1);
  moduleMap[id] = proxy;
  return proxy;
}

const MODULE_FN_IDENTIFIER = 'fn:';

enum ModuleExportType {
  string = 'string',
  boolean = 'boolean',
  number = 'number',
  object = 'object',
  stream = 'stream',
  array = 'array',
  void = 'void',
  fn_string = 'fn:string',
  fn_boolean = 'fn:boolean',
  fn_number = 'fn:number',
  fn_object = 'fn:object',
  fn_stream = 'fn:stream',
  fn_array = 'fn:array',
  fn_void = 'fn:void',
}

type ExportDefinition = {
  name: string;
  returnType: ModuleExportType;
  hasGetter: boolean;
  hasSetter: boolean;
};

interface ExportDefinitionMap {
  [key: string]: ExportDefinition;
}

async function createProxy(id: string, exportDefinitions: ExportDefinition[]) {
  const obj = Object.create({
    updateValue(name: string, value: any) {
      return (obj[name] = value);
    },
  });
  const propMap = exportDefinitions.reduce(
    (map, definition) => ({ ...map, [definition.name]: definition }),
    Object.create(null) as ExportDefinitionMap
  );
  return new Proxy(obj, {
    get(target, prop, receiver) {
      // we should only be setting the value when the type is handled in a special case
      if (typeof target[prop] !== 'undefined') {
        return target[prop];
      }
      if (typeof prop === 'symbol') {
        return Reflect.get(target, prop, receiver);
      }
      const definition = propMap[prop];
      if (typeof definition === 'undefined') {
        return undefined;
      }
      if (definition.hasGetter) {
        if (definition.returnType.startsWith(MODULE_FN_IDENTIFIER)) {
          return async function (...args: any[]) {
            const result = await sendMessage('INVOKE_MODULE_METHOD', {
              id,
              method: prop,
              args,
            });
            switch (definition.returnType) {
              case ModuleExportType.fn_stream:
                return new InteropStream(result);
              default:
                return result;
            }
          };
        } else {
          if (typeof target[prop] !== 'undefined') {
            return target[prop];
          }
          const returnValue = sendMessageSync('GET_MODULE_PROP', {
            prop,
            id,
            args: {},
          });
          switch (definition.returnType) {
            case ModuleExportType.stream:
              // if it's a prop, it gets memoized in the source object
              return (target[prop] = new InteropStream(returnValue));
            default:
              return (target[prop] = returnValue);
          }
        }
      }
    },
    set(target, prop, value, _) {
      if (typeof prop === 'string' && typeof propMap[prop] !== 'undefined') {
        const didUpdate = sendMessageSync<boolean>('SET_MODULE_PROP', {
          value,
        });
        if (!didUpdate) {
          throw `Failed to update property ${prop}, check logs`;
        }
      }
      return (target[prop] = value);
    },
    deleteProperty(target, p) {
      if (typeof p === 'string' && typeof propMap[p] !== 'undefined') {
        const prop = propMap[p];
        // we need to close the stream before deletion
        if (prop.returnType === ModuleExportType.stream) {
          const stream = target[p] as InteropStream;
          stream.close();
          return delete target[p];
        }
      }
      return false;
    },
    has(target, prop) {
      return Object.hasOwn(target, prop);
    },
  });
}

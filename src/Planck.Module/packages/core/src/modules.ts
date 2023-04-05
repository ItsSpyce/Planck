import { InteropStream } from 'stream.js';
import { sendMessage, sendMessageSync } from 'messages.js';

const moduleMap = Object.create(null) as Record<
  string,
  Awaited<ReturnType<typeof createProxy>>
>;

export async function _import<TModule>(id: string) {
  if (id in moduleMap) {
    return moduleMap[id] as TModule;
  }
  const moduleConfiguration = await sendMessage<ExportDefinition[]>(
    'LOAD_MODULE',
    { id }
  );
  const proxy = await createProxy(id, moduleConfiguration);
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
  const obj = Object.create(null);
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
          const returnValue = sendMessageSync('GET_MODULE_PROP', {
            prop,
            id,
            args: {},
          });
          switch (definition.returnType) {
            case ModuleExportType.stream:
              // if it's a prop, it gets memoized in the source object
              return new InteropStream(returnValue);
            default:
              return returnValue;
          }
        }
      }
    },
    set(target, prop, value, _) {
      if (typeof prop === 'string' && typeof propMap[prop] !== 'undefined') {
        throw 'Cannot set the value on a remote module property';
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

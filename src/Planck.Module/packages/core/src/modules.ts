import {
  postMessageAndWait,
  createOperationIdFactory,
  postMessageAndWaitSync,
} from 'utils.js';

const operationIdFactory = createOperationIdFactory();
const moduleMap = Object.create(null) as Record<
  string,
  Awaited<ReturnType<typeof createProxy>>
>;

export async function _import<TModule>(id: string) {
  if (id in moduleMap) {
    return moduleMap[id] as TModule;
  }
  const [moduleConfiguration] = await postMessageAndWait<ExportDefinition[]>(
    { command: 'LOAD_MODULE', body: { id } },
    operationIdFactory.next()
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
  fn_string = `${MODULE_FN_IDENTIFIER}:${ModuleExportType.string}`,
  fn_boolean = `${MODULE_FN_IDENTIFIER}:${ModuleExportType.boolean}`,
  fn_number = `${MODULE_FN_IDENTIFIER}:${ModuleExportType.number}`,
  fn_object = `${MODULE_FN_IDENTIFIER}:${ModuleExportType.object}`,
  fn_stream = `${MODULE_FN_IDENTIFIER}:${ModuleExportType.stream}`,
  fn_array = `${MODULE_FN_IDENTIFIER}:${ModuleExportType.array}`,
  fn_void = `${MODULE_FN_IDENTIFIER}:${ModuleExportType.void}`,
}

// type ModuleExportType =
//   | 'string'
//   | 'boolean'
//   | 'number'
//   | 'object'
//   | 'stream'
//   | 'array'
//   | 'void'
//   | `fn:${
//       | 'string'
//       | 'boolean'
//       | 'number'
//       | 'object'
//       | 'stream'
//       | 'array'
//       | 'void'}`;

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
      if (typeof prop === 'symbol') {
        return Reflect.get(target, prop, receiver);
      }
      const definition = propMap[prop];
      if (typeof definition === 'undefined') {
        return undefined;
      }
      if (definition.hasGetter) {
        if (definition.returnType.startsWith(MODULE_FN_IDENTIFIER)) {
          switch (definition.returnType) {
            case ModuleExportType.fn_stream:
              // TODO:
              throw new Error('Not implemented');
            default:
              return async function (...args: any[]) {
                const [response] = await postMessageAndWait(
                  {
                    command: 'INVOKE_MODULE_METHOD',
                    body: { id, method: prop, args },
                  },
                  operationIdFactory.next()
                );
                return response;
              };
          }
        }
        return postMessageAndWaitSync(
          { command: 'GET_MODULE_PROP', body: { prop, id, args: {} } },
          operationIdFactory.next()
        );
      }
    },
    deleteProperty() {
      return false;
    },
    has(target, prop) {
      return Object.hasOwn(target, prop);
    },
  });
}

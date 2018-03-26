declare module '*.vue' {
  import Vue, { ComponentOptions} from 'vue';
  const value: ComponentOptions<Vue>;
  export default value;
}

declare module 'bootstrap-vue' {
  import Vue, { PluginObject } from 'vue';
  const value: PluginObject<any>;
  export default value;
}

declare module 'bootstrap-vue/es/directives/popover/popover' {
  import Vue, { DirectiveOptions } from 'vue';
  const value: DirectiveOptions;
  export default value;
}

<template>
  <div>
    <username-input-component v-on:input="logUsername"></username-input-component>
  </div>
</template>

<script lang="ts">
import Vue from 'vue';
import UsernameInputComponent from './username-input.vue';

export default Vue.extend({
  components: {
    UsernameInputComponent,
  },
  data() {
    return {
      username: '',
    }
  },
  methods: {
    logUsername(event: any) {
      debugger;
    }
  }
})
</script>

<style lang="scss" scoped>
</style>


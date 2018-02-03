<template>
  <div>
    <span>Test text</span>
    <input type="text" @input="handleInputChange($event.target.value)"/>
  </div>
</template>

<script lang="ts">
import Vue from 'vue';
import Component from 'vue-class-component';

@Component({})
export default class UsernameInput extends Vue {
  username: string = '';

  handleInputChange(value: any) {
    this.username = value;
    this.$emit('input', value)
  }
}

/* export default Vue.extend({
  data() {
    return {
      username: '',
    }
  },
  methods: {
    handleInputChange(event) {
      this.$emit('usernameChange', event.target.value);
    }
  }
}) */
</script>

<style lang="scss" scoped>
span {
  color: aqua;
}
</style>


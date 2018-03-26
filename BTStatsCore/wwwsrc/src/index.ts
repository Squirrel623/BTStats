import Vue from 'vue';

import BootstrapVue from 'bootstrap-vue';
import 'bootstrap/dist/css/bootstrap.css'
import 'bootstrap-vue/dist/bootstrap-vue.css'

import BtUserStats from './bt-user-stats.vue'

Vue.use(BootstrapVue);

const app = new Vue({
  el: '#app',
  components: {
    BtUserStats
  }
});
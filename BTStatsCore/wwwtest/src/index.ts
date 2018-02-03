import 'bootstrap/dist/css/bootstrap.min.css';
import Vue from 'vue';
import MainComponent from './components/main.vue';

let v = new Vue({
  el: '#app',
  template: '<main-component/>',
  components: {
    MainComponent
  }
});
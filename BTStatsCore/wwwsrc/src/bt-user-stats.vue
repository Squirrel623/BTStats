<template>
<div class="bt-user-stats-container">
  <div class="title"><h1 class="center-flex">BT Log Stats</h1></div>

  <div class="calendar">
    <heatmap-carousel :username="username"></heatmap-carousel>
  </div>

  <div class="username">
    <typeahead :items="usernames" @selected="commitUsername" placeholder="Enter Username"></typeahead>
  </div>

  <div class="stats">
    <div class="stat-item card">
      <div class="card-header">
        <h2 class="login-date-header">First Login Date</h2>
        <span class="fa fa-info-circle" v-b-popover.nofade.hover="'Logs start on 2014-01-09'"></span>
      </div>
      <div class="card-body"><h4>{{firstLogin}}</h4></div>
    </div>
    <div class="stat-item card">
      <div class="card-header">
        <h2 class="login-date-header">Last Login Date</h2>
        <span class="fa fa-info-circle" v-b-popover.nofade.hover="'Data has up to 24 hour lag'"></span>
      </div>
      <div class="card-body">
        <h4>{{lastLogin}}</h4>
      </div>
    </div>
    <div class="stat-item card">
      <div class="card-header"><h2># of Times Logged In</h2></div>
      <div class="card-body"><h4>{{loginCount}} Logins</h4></div>
    </div>
    <div class="stat-item card">
      <div class="card-header"><h2>Total Logged In Time</h2></div>
      <div class="card-body"><h4>{{loginTimeTotal}} Days</h4></div>
    </div>
    <div class="stat-item card">
      <div class="card-header"><h2>Total # Messages</h2></div>
      <div class="card-body"><h4>{{messagesTotal}} Messages</h4></div>
    </div>
    <div class="stat-item card">
      <div class="card-header"><h2>Most Used Emotes</h2></div>
      <div id="emote-table" class="card-body">
        <table class="table">
          <thead>
          <tr>
            <th>#</th>
            <th>Emote</th>
            <th># of Times Used</th>
          </tr>
          </thead>
          <tbody>
          <tr v-for="(emote, index) in mostUsedEmotes">
            <td>{{index+1}}</td>
            <td>{{emote.name}}</td>
            <td>{{emote.times}}</td>
          </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</div>
</template>

<script lang="ts">
import Vue from 'vue';
import {map} from 'lodash-es';
import * as $ from 'jquery';

import vBPopover from 'bootstrap-vue/es/directives/popover/popover';
import typeahead from './components/Typeahead.vue';
import heatmapCarousel from './components/HeatmapCarousel.vue';

Vue.directive('b-popover', vBPopover);

import UserDataStore from './user-data-store';
import {get} from './util/ajax';

export default Vue.extend({
  components: {
    typeahead,
    heatmapCarousel,
  },
  data() {
    return {
      username: '',
      usernames: [],
      firstLogin: '',
      lastLogin: '',
      loginCount: 0,
      loginTimeTotal: 0,
      messagesTotal: 0,
      mostUsedEmotes: []
    }
  },
  created() {
    get('/users').then((result: any) => {
      this.usernames = result.data;
    })
  },
  mounted() {
    this.$nextTick(() => {
      $(this.$el).find('.typeahead-input').focus();
    });
  },
  methods: {
    commitUsername(username: string) {
      this.username = username;
      this.updateUsernameData();
    },
    updateUsernameData() {
      var prom1 = UserDataStore.getLoginCount(this.username)
        .then((count: number) => this.loginCount = count, () => this.loginCount = 0);
      var prom2 = UserDataStore.getLoggedInTime(this.username)
        //@ts-ignore
        .then((seconds: number) => this.loginTimeTotal = (seconds / 60 / 60 / 24).toFixed(1),
              () => this.loginCount = 0);
      var prom3 = UserDataStore.getTotalMessages(this.username)
        .then((messageCount: number) => this.messagesTotal = messageCount, () => this.messagesTotal = 0);
      var prom4 = UserDataStore.getMostUsedEmotes(this.username)
        //@ts-ignore
        .then((emotes: any) => this.mostUsedEmotes = map<any, any>(emotes, (value, key) => { return {name: key, times: value} }), () => this.mostUsedEmotes = []);
      var prom5 = UserDataStore.getFirstLogin(this.username)
        .then((date: string) => this.firstLogin = date, () => this.firstLogin = "");
      var prom6 = UserDataStore.getLastLogin(this.username)
        .then((date: string) => this.lastLogin = date, () => this.lastLogin = "");
    }
  }
})
</script>

<style lang="scss">
  $main-width: 800px;

  .bt-user-stats-container {
    width: 100%;
    height: 100%;
    display: grid;

    grid-template-areas: 
      ".  title    title    ."
      ".  calendar calendar ."
      ".  username username ."
      ".  stats    stats    .";

    grid-template-rows: auto auto auto auto;
    grid-template-columns: 1fr $main-width/2 $main-width/2 1fr;

    justify-items: center;
  }

  .center-flex {
    display: flex;
    justify-content: center;
  }

  .title {
    grid-area: title;
  }
  .calendar {
    grid-area: calendar;
  }
  .username {
    grid-area: username;
    justify-self: left;

    padding-top: 15px;
  }

  .stats {
    width: 100%;
    grid-area: stats;
    justify-self: left;

    padding-top: 15px;

    display: flex;
    flex-wrap: wrap;

    .stat-item {
      flex: 1 0 50%;
      box-sizing: border-box;
    }
  }

  .login-date-header {
    display: inline-block;
    padding-right: 10px;
  }
</style>


import tpl from './bt-stats.tpl';
import './bt-stats.scss';

import {map, flatMap, range, chunk, isString, includes} from 'lodash-es';
import {init, getLoginCount} from './user-data-store';

function StatsCtrl(userDataStoreService, $http, $scope){
  const $ctrl = this;

  $ctrl.usernames = [];
  $http.get('/users').then(result => {
    return $ctrl.usernames = result.data
  });

  $ctrl.sortUsernames = function(u1, u2) {
    if (u1.type !== 'string' || u2.type !== 'string') {
      return (v1.index < v2.index) ? -1 : 1;
    }
    return u1.value.length < u2.value.length ? -1 : 1;
  }

  $ctrl.$onInit = function() {
    $ctrl.typeaheadValue = $ctrl.username = '';
    $ctrl.loginCount = $ctrl.loginTimeTotal = $ctrl.messagesTotal = 0;
    $ctrl.mostUsedEmotes = [];
    $ctrl.firstLogin = "";

    $ctrl.monthYearGroups = chunk(flatMap(range(2014, 2018), year => map(range(1, 13), month => {
      return {
        year: year,
        month: month,
      }
    })),6);
    $ctrl.monthYearGroups.push([{
      year: 2018,
      month: 1
    }, {
      year: 2018,
      month: 2
    }, {
      year: 2018,
      month: 3
    }, {
      year: 2018,
      month: 4
    }, {
      year: 2018,
      month: 5
    }, {
      year: 2018,
      month: 6
    }]);

    $ctrl.activeIndex = $ctrl.monthYearGroups.length - 1;
    $ctrl.year = $ctrl.monthYearGroups[$ctrl.activeIndex][0].year;
    $ctrl.offset = -4;
  }

  
  $ctrl.$doCheck = function() {
    const year = $ctrl.monthYearGroups[$ctrl.activeIndex][0].year;
    if (year !== $ctrl.year) {
      $ctrl.year = year;
    }
  }

  $ctrl.commitUsername = function(username) {
    if (includes($ctrl.usernames, username)) {
      $ctrl.username = username;
      $ctrl.updateUsernameData();
    }
  }

  $ctrl.updateUsernameData = function() {
    var prom1 = userDataStoreService.getLoginCount($ctrl.username)
      .then(count => $ctrl.loginCount = count, () => $ctrl.loginCount = 0);
    var prom2 = userDataStoreService.getLoggedInTime($ctrl.username)
      .then(seconds => $ctrl.loginTimeTotal = (seconds / 60 / 60 / 24).toFixed(1),
        () => $ctrl.loginCount = 0);
    var prom3 = userDataStoreService.getTotalMessages($ctrl.username)
      .then(messageCount => $ctrl.messagesTotal = messageCount, () => $ctrl.messagesTotal = 0);
    var prom4 = userDataStoreService.getMostUsedEmotes($ctrl.username)
      .then(emotes => $ctrl.mostUsedEmotes = map(emotes, (value, key) => { return {name: key, times: value} }), $ctrl.mostUsedEmotes = []);
    var prom5 = userDataStoreService.getFirstLogin($ctrl.username)
      .then(date => $ctrl.firstLogin = date, () => $ctrl.firstLogin = "");

    Promise.all([prom1, prom2, prom3, prom4, prom5]).catch().then(() => $scope.$applyAsync());
  }
}

StatsCtrl.$inject = ['userDataStoreService', '$http', '$scope'];

export default {
  template: tpl,
  controller: StatsCtrl
};

export const name = 'btStats';

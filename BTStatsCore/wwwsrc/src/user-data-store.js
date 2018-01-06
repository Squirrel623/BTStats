import {has, transform, setWith, get, isEmpty} from 'lodash-es';

String.prototype.lpad = function (padString, length) {
  var str = this;
  while (str.length < length)
    str = padString + str;
  return str;
}

function UserDataStore($http) {
  const usernames = {};

  function readyUsername(username) {
    usernames[username] = usernames[username] || {};
  }

  function getSimpleValue({username, endpoint}) {
    readyUsername(username);

    if (has(usernames[username], endpoint)) {
      return Promise.resolve(usernames[username][endpoint]);
    }

    return $http.get(`/${endpoint}/${username}`).then(result => {
      usernames[username][endpoint] = result.data;
      return result.data;
    });
  }

  function getLoginCount(username) {
    return getSimpleValue({
      username: username,
      endpoint: 'loginCount'
    });
  }

  function getLoggedInTime(username) {
    return getSimpleValue({
      username: username,
      endpoint: 'loggedInTime'
    });
  }

  function getTotalMessages(username) {
    return getSimpleValue({
      username: username,
      endpoint: 'messageCount'
    });
  }

  function getLoginTimePerDay({username, year, month, offset=0}) {
    readyUsername(username);
    const user = usernames[username];

    if(!has(user, 'loginTimePerDay')) {
      user.loginTimePerDay = {};
    }

    if(has(user.loginTimePerDay,offset)) {
      return user.loginTimePerDay[offset].then(result => get(result, [year.toString().lpad('0', 2), month.toString().lpad('0', 2)], {}));
    }

    user.loginTimePerDay[offset] = $http.get(`/loggedInTimePerDay/${offset}/${username}`).then(result => {
      return transform(result.data, (acc, tuple) => setWith(acc, tuple.date.split('-'), tuple.milliseconds, Object), {});
    });
    return user.loginTimePerDay[offset].then(result => get(result, [year.toString().lpad('0', 2) ,month.toString().lpad('0', 2)], {}));
  }

  function getMostUsedEmotes(username) {
    readyUsername(username);

    if (has(usernames[username], 'emotes')) {
      return Promise.resolve(usernames[username].emotes);
    }

    return $http.get(`/emotes/${username}/5`).then(result => {
      usernames[username].emotes = result.data;
      return result.data;
    });
  }

  return {
    getLoginCount,
    getLoggedInTime,
    getLoginTimePerDay,
    getTotalMessages,
    getMostUsedEmotes,
  }
}

UserDataStore.$inject = ['$http'];

export const name = 'userDataStoreService';
export default UserDataStore;

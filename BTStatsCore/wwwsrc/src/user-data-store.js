import {has, transform, setWith, get, isEmpty} from 'lodash-es';

String.prototype.lpad = function (padString, length) {
  var str = this;
  while (str.length < length)
    str = padString + str;
  return str;
}

const http = {
  get: function(endpoint) {
    return new Promise((resolve, reject) => {
      $.ajax({
        url: endpoint,
        type: 'GET',
        contentType: 'application/json',
        dataType: 'json',
        global: false,
        complete: (response, status) => {
          if (status !== 'success') {
            reject();
          }
          resolve({
            data: response.responseJSON,
          });
        },
      })
    });
  }
}

function UserDataStore() {
  const usernames = {};

  function readyUsername(username) {
    usernames[username] = usernames[username] || {};
  }

  function getSimpleValue({username, endpoint}) {
    readyUsername(username);

    if (has(usernames[username], endpoint)) {
      return Promise.resolve(usernames[username][endpoint]);
    }

    return http.get(`/${endpoint}/${username}`).then(result => {
      usernames[username][endpoint] = result.data;
      return result.data;
    });
  }

  function getStaticData({username, endpoint}) {
    readyUsername(username);

    let promise = has(usernames[username], 'staticDataPromise') ?
      usernames[username].staticDataPromise :
      http.get(`/allStaticData/${username}`);

    usernames[username].staticDataPromise = promise;
    return promise.then(result => result.data[endpoint]);
  }

  function getLoginCount(username) {
    return getStaticData({
      username: username,
      endpoint: 'loginCount'
    });
  }

  function getLoggedInTime(username) {
    return getStaticData({
      username: username,
      endpoint: 'loggedInTime'
    });
  }

  function getTotalMessages(username) {
    return getStaticData({
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

    user.loginTimePerDay[offset] = http.get(`/loggedInTimePerDay/${offset}/${username}`).then(result => {
      return transform(result.data, (acc, tuple) => setWith(acc, tuple.date.split('-'), tuple.milliseconds, Object), {});
    });
    return user.loginTimePerDay[offset].then(result => get(result, [year.toString().lpad('0', 2) ,month.toString().lpad('0', 2)], {}));
  }

  function getMostUsedEmotes(username) {
    return getStaticData({
      username: username,
      endpoint: 'emotes',
    })
  }

  function getFirstLogin(username) {
    return getStaticData({
      username: username,
      endpoint: 'firstLogin'
    });
  }

  function getLastLogin(username) {
    return getStaticData({
      username: username,
      endpoint: 'lastLogin'
    });
  }

  return {
    getLoginCount,
    getLoggedInTime,
    getLoginTimePerDay,
    getTotalMessages,
    getMostUsedEmotes,
    getFirstLogin,
    getLastLogin,
  }
}

UserDataStore.$inject = [];

export const name = 'userDataStoreService';
export default UserDataStore;

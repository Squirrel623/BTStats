import {has, transform, setWith, get, isEmpty} from 'lodash-es';
import {get as ajaxGet} from './util/ajax';

String.prototype.lpad = function (padString: string, length: number): string {
  var str = this;
  while (str.length < length)
    str = padString + str;
  return str as string;
}

export class UserDataStore {
  private usernames: {[index: string]: any} = {};

  private readyUsername(username: string) {
    this.usernames[username] = this.usernames[username] || {};
  }

  private getSimpleValue({username, endpoint}: {username: string, endpoint: string}) {
    this.readyUsername(username);

    if (has(this.usernames[username], endpoint)) {
      return Promise.resolve(this.usernames[username][endpoint]);
    }

    return ajaxGet(`/${endpoint}/${username}`).then((result: any) => {
      this.usernames[username][endpoint] = result.data;
      return result.data;
    });
  }

  private getStaticData({username, endpoint}: {username: string, endpoint: string}) {
    this.readyUsername(username);

    let promise = has(this.usernames[username], 'staticDataPromise') ?
      this.usernames[username].staticDataPromise :
      ajaxGet(`/allStaticData/${username}`);

    this.usernames[username].staticDataPromise = promise;
    return promise.then((result: any) => result.data[endpoint]);
  }

  public getLoginCount(username: string): Promise<number> {
    return this.getStaticData({
      username: username,
      endpoint: 'loginCount'
    });
  }

  public getLoggedInTime(username: string): Promise<number> {
    return this.getStaticData({
      username: username,
      endpoint: 'loggedInTime'
    });
  }

  public getTotalMessages(username: string): Promise<number> {
    return this.getStaticData({
      username: username,
      endpoint: 'messageCount'
    });
  }

  public getLoginTimePerDay({username, year, month, offset=0}: {username: string, year: number, month: number, offset: number}): Promise<any> {
    this.readyUsername(username);
    const user = this.usernames[username];

    if(!has(user, 'loginTimePerDay')) {
      user.loginTimePerDay = {};
    }

    if(has(user.loginTimePerDay, offset)) {
      return user.loginTimePerDay[offset].then((result: any) => get(result, [year.toString().lpad('0', 2), month.toString().lpad('0', 2)], {}));
    }

    user.loginTimePerDay[offset] = ajaxGet(`/loggedInTimePerDay/${offset}/${username}`).then((result: any) => {
      return transform(result.data, (acc: any, tuple: any) => setWith(acc, tuple.date.split('-'), tuple.milliseconds, Object), {});
    });
    return user.loginTimePerDay[offset].then((result: any) => get(result, [year.toString().lpad('0', 2) ,month.toString().lpad('0', 2)], {}));
  }

  public getMostUsedEmotes(username: string): Promise<any> {
    return this.getStaticData({
      username: username,
      endpoint: 'emotes',
    })
  }

  public getFirstLogin(username: string): Promise<string> {
    return this.getStaticData({
      username: username,
      endpoint: 'firstLogin'
    });
  }

  public getLastLogin(username: string): Promise<string> {
    return this.getStaticData({
      username: username,
      endpoint: 'lastLogin'
    });
  }
}

export default new UserDataStore();

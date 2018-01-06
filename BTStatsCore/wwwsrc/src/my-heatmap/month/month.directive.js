import tpl from './month.tpl';
import './month.scss';

import {range, forEach, isString, isPlainObject, isFinite} from 'lodash-es';

import calendar from '../../calendar';
import {intToString as monthIntToString} from './month-constant';

function MonthController(userDataStoreService) {
  const ctrl = this;

  

}
MonthController.$inject = ['userDataStoreService'];


const rows = range(6);
const columns = range(7);

/**
 *  0-0 0-1 0-2 0-3 0-4 0-5 0-6
 *  1-0 1-1 1-2 1-3 1-4 1-5 1-6
 *  ...
 * 
 */

 //StartDay: 3
 //cols.length: 7
 //1-4 -> 9
 //0-5 -> 3

function getDayToCssClassMap({startDay, daysInMonth}) {
  const map = {};
  const rowColToDayMap = {};

  let day = 1;
  let daysLeft = daysInMonth

  forEach(rows, row => {
    forEach(columns, col => {
      const i = row * 7 + col;
      if (i < startDay || day > daysInMonth) {
        return;
      }

      const dayString = day.toString().lpad('0', 2);
      map[dayString] = `.gc-${row}-${col}`;
      rowColToDayMap[`${row}-${col}`] = dayString;
      day++;
    });
  });

  return {
    dayToCss: map,
    rowColToDay: rowColToDayMap,
  };
}

function MonthDirective(userDataStoreService, $http) {
  return {
    template: tpl,
    scope: {
      username: '<',
      offset: '<',
      year: '<',
      month: '<'
    },
    //controller: MonthController,
    //bindToController: true,
    //controllerAs: '$ctrl',
    link: function($scope, $element) {
      $scope.monthToStringMap = monthIntToString;

      $scope.rowCols =[];
      forEach(rows, row => {
        forEach(columns, col => {
          $scope.rowCols.push({ r: row, c: col });
        });
      });

      $scope.offset = $scope.offset || 0;

      const monthCalendar = calendar[$scope.year][$scope.month];
      const startDay = monthCalendar.dayOfWeekStart;
      const maps = getDayToCssClassMap({
        startDay: startDay,
        daysInMonth: monthCalendar.daysInMonth,
      });

      $scope.$applyAsync(() => {
        forEach(maps.dayToCss, (value, day) => {
          const cell = $element.find(value);
          cell.css('visibility', 'visible');
          cell.text(day);
        });
      });

      function resetCalendar() {
        forEach(maps.dayToCss, (value, day) => {
          $element.find(value).css('background-color', 'white');
        });
      }

      $scope.getHourValue = function(row, column) {
        if (!isPlainObject($scope.dataMap)) {
          return 0;
        }

        const day = maps.rowColToDay[`${row}-${column}`];
        if (!isString(day)) {
          return 0;
        }

        const value = $scope.dataMap[day]
        if (!isFinite(value)) {
          return 0;
        }

        return (value / 1000 / 60 / 60).toFixed(1);
      }
      
      function updateData() {
        userDataStoreService.getLoginTimePerDay({
          offset: $scope.offset,
          username: $scope.username,
          year: $scope.year,
          month: $scope.month
        }).then(daysObj => {
          resetCalendar();
          $scope.dataMap = daysObj;
          forEach(daysObj, (milliseconds, day) => {
            const cssSelector = maps.dayToCss[day];
            if (!isString(cssSelector)) {
              return;
            }

            const hours = milliseconds / 1000 / 60 / 60;
            const gridItem = $element.find(cssSelector);

            if (hours < 2) {
              gridItem.css('background-color', 'lightgreen');
            } else if (hours < 4) {
              gridItem.css('background-color', 'green');
            } else if (hours < 6) {
              gridItem.css('background-color', 'yellow');
            } else if (hours < 10) {
              gridItem.css('background-color', 'orange');
            } else if (hours < 16) {
              gridItem.css('background-color', 'tomato');
            } else {
              gridItem.css('background-color', 'red');
            }
          });
        })
      }

      $scope.$watch('username', newUsername => {
        if (newUsername === '') return;

        updateData();
      });

      let init = false;
      $scope.$watch('offset', offset => {
        if (!init || $scope.username === '') {
          init = true;
          return;
        }
        
        updateData();
      });
    }
  }
}

MonthDirective.$inject = ['userDataStoreService', '$http'];

export default MonthDirective;

export const name = 'heatmapMonth';
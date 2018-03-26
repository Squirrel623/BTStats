<template>
<div class="heatmap-month">
  <div class="month-container">
    <div class="header" v-once>{{monthToStringMap[month]}}</div>
    <div v-for="rowCol in rowCols"
        :class="'gc-' + rowCol.r + '-' + rowCol.c"
        v-b-popover.nofade.hover="getHourValue(rowCol.r, rowCol.c)">
    </div>
  </div>
</div>
</template>

<script>
import Vue from 'vue';
import vBPopover from 'bootstrap-vue/es/directives/popover/popover';
import {range, forEach, isString, isPlainObject, isFinite} from 'lodash-es';
import * as $ from 'jquery';

import calendar from '../calendar';
import {intToString as monthIntToString} from '../my-heatmap/month/month-constant';
import UserDataStore from '../user-data-store';

Vue.directive('b-popover', vBPopover);

const rows = range(6);
const columns = range(7);

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

export default {
  props: {
    username: String,
    offset: Number,
    year: Number,
    month: Number
  },
  data() {
    return {
      monthToStringMap: monthIntToString,
      rowCols: [],
      monthCalendar: {},
      startDay: 0,
      maps: {},
      dataMap: {},
      jEl: null
    }
  },
  mounted() {
    this.jEl = $(this.$el);

    forEach(rows, row => {
      forEach(columns, col => {
        this.rowCols.push({ r: row, c: col });
      });
    });

    this.monthCalendar = calendar[this.year][this.month];
    this.startDay = this.monthCalendar.dayOfWeekStart;
    this.maps = getDayToCssClassMap({
      startDay: this.startDay,
      daysInMonth: this.monthCalendar.daysInMonth,
    });

    this.$nextTick(() => {
      forEach(this.maps.dayToCss, (value, day) => {
        const cell = this.jEl.find(value);
        cell.css('visibility', 'visible');
        cell.text(day);
      });
    })
  },
  methods: {
    resetCalendar() {
      forEach(this.maps.dayToCss, (value, day) => {
        this.jEl.find(value).css('background-color', 'white');
      });
    },
    getHourValue(row, column) {
      const defaultValue = "0 Hours";
      if (!isPlainObject(this.dataMap)) {
        return defaultValue;
      }

      const day = this.maps.rowColToDay[`${row}-${column}`];
      if (!isString(day)) {
        return defaultValue;
      }

      const value = this.dataMap[day]
      if (!isFinite(value)) {
        return defaultValue;
      }

      return `${(value / 1000 / 60 / 60).toFixed(1)} Hours`;
    },
    updateData() {
      UserDataStore.getLoginTimePerDay({
        offset: this.offset,
        username: this.username,
        year: this.year,
        month: this.month
      }).then(daysObj => {
        this.resetCalendar();
        this.dataMap = daysObj;
        forEach(daysObj, (milliseconds, day) => {
          const cssSelector = this.maps.dayToCss[day];
          if (!isString(cssSelector)) {
            return;
          }

          const hours = milliseconds / 1000 / 60 / 60;
          const gridItem = this.jEl.find(cssSelector);

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
  },
  watch: {
    username(newUsername) {
      if (newUsername === '') return;

      this.updateData();
    },
    offset() {
      if (this.username === '') {
        return;
      }

      this.updateData();
    }
  }
}
</script>

<style lang="scss">
.heatmap-month {
  display: inline-block;
  width: 200px;
  height: 200px;

  .month-container {
    width: 100%;
    height: 100%;

    display: grid;
    justify-content: center;
    align-content: center;

    background-color: lightgray;

    grid-auto-columns: 20px;
    grid-auto-rows: 20px;
    grid-row-gap: 5px;
    grid-column-gap: 5px;

    grid-template-areas:
     "header header header header header header header"
     "gc-0-0 gc-0-1 gc-0-2 gc-0-3 gc-0-4 gc-0-5 gc-0-6"
     "gc-1-0 gc-1-1 gc-1-2 gc-1-3 gc-1-4 gc-1-5 gc-1-6"
     "gc-2-0 gc-2-1 gc-2-2 gc-2-3 gc-2-4 gc-2-5 gc-2-6"
     "gc-3-0 gc-3-1 gc-3-2 gc-3-3 gc-3-4 gc-3-5 gc-3-6"
     "gc-4-0 gc-4-1 gc-4-2 gc-4-3 gc-4-4 gc-4-5 gc-4-6"
     "gc-5-0 gc-5-1 gc-5-2 gc-5-3 gc-5-4 gc-5-5 gc-5-6";
    
  }

  div[class *= "gc"] {
    user-select: none;
    visibility: hidden;
    background-color: white;
    display: flex;
    justify-content: center;
    align-items: center;
    color: #333;
  }

  .header {
    grid-area: header;
    color: #333;
  }
}
</style>


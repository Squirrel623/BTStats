<template>
<div class="bt-carousel">
  <h2>{{year}}</h2>
  <b-carousel v-model="activeIndex" :interval="0" :indicators="false" :controls="true" img-width="800" img-height="400">
    <b-carousel-slide
       v-for="(group, index) in monthYearGroups" :key="index"
       :img-blank="true">
      <bt-heatmap-month
        v-for="(monthYear, monthIndex) in group" :key="monthIndex"
        :username="username"
        :year="monthYear.year"
        :month="monthYear.month"
        :offset="offset">
      </bt-heatmap-month>
    </b-carousel-slide>
  </b-carousel>
  <b-button-group class="btn-group center-flex">
    <b-button class="offset-button" v-for="btn in buttons" @click="setBtnOffset(btn)" :key="btn.offset" :pressed="btn.state">
      {{btn.label}}
    </b-button>
  </b-button-group>
</div>
</template>

<script>
import * as $ from 'jquery';
import {map, flatMap, range, chunk, isString, includes, forEach} from 'lodash-es';

import bCarousel from 'bootstrap-vue/es/components/carousel/carousel';
import bCarouselSlide from 'bootstrap-vue/es/components/carousel/carousel-slide';
import bButton from 'bootstrap-vue/es/components/button/button';
import bButtonGroup from 'bootstrap-vue/es/components/button-group/button-group';

import btHeatmapMonth from './HeatmapMonth.vue';

const now = new Date();
const currentYear = now.getFullYear();
const shouldFillLastHalfOfLastYear = now.getMonth() > 5;
const monthYearGroups = [];

function getGroupForMonths(year, start, end) {
  return map(range(start, end), month => {
    return {
      year: year,
      month: month,
    }
  });
}

for (let year = 2014; year <= currentYear; year++) {
  monthYearGroups.push(getGroupForMonths(year, 1, 7));

  if (year < currentYear || shouldFillLastHalfOfLastYear) {
    monthYearGroups.push(getGroupForMonths(year, 7, 13));
  }
}

export default {
  props: {
    username: String,
  },
  components: {
    bCarousel,
    bCarouselSlide,
    bButton,
    bButtonGroup,
    btHeatmapMonth,
  },
  data() {
    const activeIndex = monthYearGroups.length - 1;

    return {
      monthYearGroups: monthYearGroups,
      activeIndex: activeIndex,
      offset: -4,
      buttons: [
        {label: 'Pacific', offset: -7, state: false},
        {label: 'Mountain', offset:-6, state: false},
        {label: 'Central', offset:-5, state: false},
        {label: 'Eastern', offset:-4, state: true},
        {label: 'London', offset:0, state: false},
        {label: 'Berlin', offset:1, state: false},
        {label: 'Athens', offset:2, state: false},
        {label: 'UTC+8', offset:8, state: false},
        {label: 'UTC+9', offset:9, state: false},
      ]
    };
  },
  methods: {
    setBtnOffset(button) {
      forEach(this.buttons, (btn) => {
        btn.state = false
      });
      button.state = true;
      this.offset = button.offset;
    }
  },
  computed: {
    year: function() {
      return monthYearGroups[this.activeIndex][0].year;
    }
  }
}
</script>

<style lang="scss">
  $main-width: 800px;

  .offset-button {
    box-shadow: none !important;
  }

  .carousel-item {
    outline: none;
    img {
      display: none !important;
    }
    .carousel-caption {
      position: relative;
      width: 100%;
      height: 100%;
      right: initial;
      bottom: initial;
      left: initial;
      padding-top: 0px;
      padding-bottom: 0px;
    }
  } 

  .carousel {
    width: $main-width;
    height: 400px;
    display: flex;
    justify-content: center;
  }
  .carousel-inner {
    width: 80%;
  }
  .carousel-control-next,
  .carousel-control-prev {
    width: 10%;
  }
  .carousel-control-next {
    background-image: linear-gradient(to right, rgba(0,0,0,0.0001) 0, rgba(0,0,0,0.5) 100%);
  }
  .carousel-control-prev {
    background-image: linear-gradient(to right, rgba(0,0,0,0.5) 0, rgba(0,0,0,0.0001) 100%);
  }

</style>


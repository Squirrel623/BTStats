<template>
  <div class="form-group typeahead">
    <input ref="input" class="form-control typeahead-input"
            type="text"
            v-model="query"
            :placeholder="placeholder"
            v-on:change="queryChanged"
            v-bind:class="{'show-dropdown-items': showDropdownItems}"
            @focus="initControl()"
            @keydown.enter="commitSelected"
            @keydown.up.stop.prevent="activeIndex = activeIndex > 0 ? activeIndex - 1 : 0"
            @keydown.down.stop.prevent="activeIndex = activeIndex < maxQueries-1 ? activeIndex + 1 : maxQueries-1"
            @keydown.esc="$refs.input.blur()"
            />
    <div class="typeahead-dropdown list-group"
         v-if="showDropdownItems">
      <span
       v-for="(item, index) in filtered"
       class="list-group-item"
       v-bind:class="{active: index === activeIndex}"
      @mouseup="commitSelected"
      @mouseover="activeIndex = index">
        {{item}}
      </span>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from 'vue'
import {filter, sortBy, take} from 'lodash-es'

export default Vue.extend<{showDropdownItems: boolean, query: string, maxQueries: number, activeIndex: number}, {}, {}, {items: string[], placeholder: string}>({
  props: {
    items: {
      type: Array,
      required: true
    },
    placeholder: {
      type: String,
      default: ''
    }
  },
  data() {
    return {
      showDropdownItems: false,
      query: '',
      maxQueries: 5,
      activeIndex: 0
    }
  },
  methods: {
    initControl() {
      this.query = "";
      this.showDropdownItems = true;
    },
    queryChanged() {
      //this.activeIndex = 0;
    },
    itemSelected(index: number) {
      //@ts-ignore
      this.query = this.filtered[index];
      
      console.log(this.query);
    },
    commitSelected() {
      //@ts-ignore
      this.query = this.filtered[this.activeIndex];
      //@ts-ignore
      this.$refs.input.blur();
      this.showDropdownItems = false;

      console.log("Selected: " + this.query);
      this.$emit('selected', this.query);
    }
  },
  computed: {
    filtered() {
      if (this.query.length < 1) {
        return [];
      }

      this.activeIndex = 0;
      const lowerQuery = this.query.toLowerCase();

      return take(
                sortBy(
                  filter(this.items, (item: string) => {
                    return item.toLowerCase().includes(lowerQuery);
                  }),
                  (item: string) => {
                    return item.length;
                  }
                ),
              this.maxQueries);
    }
  }
})
</script>

<style lang="scss" scoped>
  $input-border-radius: 6px;
  @mixin show-dropdown {
    z-index: 101;
    display: block
  }

  .list-group-item {
    z-index: 2;
    &.active, {
      text-decoration: none;
      color: #333;
      background-color: #eee;
      border: 1px solid rgba(0,0,0,0.125);
    }
  }

  .typeahead {
    position: relative;

    &-input {
      z-index: 1;
      position: relative;
      &.form-control {
        border-top-right-radius: $input-border-radius !important;
        border-bottom-right-radius: $input-border-radius !important;
      }

      .show-dropdown-items {
        z-index: 3;
        padding-bottom: 8px;
        height: 36px;
        border-bottom-left-radius: 0;
        border-bottom-right-radius: 0 !important;
      }
    }

    &-dropdown {
      display: block;
      position: absolute;
      top: 100%;
      left: 0;
      width: 100%;
      margin-top: -2px;
      transition-delay: 0.75s;
      transition: display 1s;

      > .list-group-item {
        &:first-child {
          border-top-left-radius: 0;
          border-top-right-radius: 0;
          &:not(:last-child) {
            border-radius: 0;
          }
        }
      }
    }
  }
</style>

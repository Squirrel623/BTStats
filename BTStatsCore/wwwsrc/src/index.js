import './style';

import angular from 'angular';
import angularAnimate from 'angular-animate';
import angularTouch from 'angular-touch';
import typeahead from 'angular-ui-bootstrap/src/typeahead';
import popover from 'angular-ui-bootstrap/src/popover';
import carousel from 'angular-ui-bootstrap/src/carousel';
import button from 'angular-ui-bootstrap/src/buttons';

import btStatsComponent,{name as btStatsComponentName} from './bt-stats.component';
import heatmapMonth, {name as heatmapMonthDirectiveName} from './my-heatmap/month/month.directive';
import userDataStore, {name as userDataStoreServiceName} from './user-data-store';

const btStatsModule = angular.module('btStats', [angularAnimate, angularTouch, typeahead, popover, carousel, button]);

btStatsModule.component(btStatsComponentName, btStatsComponent);
btStatsModule.directive(heatmapMonthDirectiveName, heatmapMonth);
btStatsModule.service(userDataStoreServiceName, userDataStore);

export default btStatsModule;

const common = require('./webpack.config.js');
const minifyPlugin = require('babel-minify-webpack-plugin');

common.plugins.push(new minifyPlugin());
common.devtool = '';

module.exports = common;
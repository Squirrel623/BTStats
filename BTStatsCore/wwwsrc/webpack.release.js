const webpack = require('webpack');
const common = require('./webpack.config.js');
const minifyPlugin = require('babel-minify-webpack-plugin');

common.plugins.push(new minifyPlugin());
common.devtool = '';

common.plugins.push(new webpack.DefinePlugin({
  'process.env.NODE_ENV': JSON.stringify('production')
}))

module.exports = common;
const webpack = require('webpack');
const common = require('./webpack.config.js');
const minifyPlugin = require('babel-minify-webpack-plugin');
const ExtractTextPlugin = require("extract-text-webpack-plugin");

common.plugins.push(new minifyPlugin());
common.devtool = '';

common.plugins.push(new webpack.DefinePlugin({
  'process.env.NODE_ENV': JSON.stringify('production')
}))

common.plugins.push(new ExtractTextPlugin('styles.css'));

module.exports = common;
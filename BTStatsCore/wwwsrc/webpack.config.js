var path = require('path');
var HtmlWebpackPlugin = require('html-webpack-plugin');
var ExtractTextPlugin = require('extract-text-webpack-plugin');

const htmlPlugin = new HtmlWebpackPlugin({
  title: 'BT Stats',
  template: './src/index.html',
})

const extractSass = new ExtractTextPlugin({
  filename: '[name]-scss.css',
})
const extractCss = new ExtractTextPlugin({
  filename: '[name].css',
})

module.exports = {
  entry: {
    site: [
      './src/index.js',
    ],
  },
  output: {
    filename: 'bundle.js',
    path: path.resolve(__dirname, '../wwwroot/'),
  },
  plugins: [htmlPlugin, extractSass, extractCss],
  module: {
    rules: [
      {
        test: /\.js$/,
        exclude: /node_modules/,
        use: {
          loader: 'babel-loader',
          options: {
            presets: ['env']
          }
        }
      },
      {
        test: /\.html$/,
        loader: 'html-loader',
      },
      {
        test: /\.css$/,
        use: extractCss.extract({
          fallback: 'style-loader',
          use: [{
            loader: 'css-loader',
            options: {sourceMap: true}
          }]
        })
      },
      {
        test: /\.scss$/,
        use: extractSass.extract({
          fallback: 'style-loader',
          use: [{
            loader: 'css-loader',
            options: {sourceMap: true}
          }, {
            loader: 'sass-loader',
            options: {sourceMap: true}
          }]
        })
      }
    ],
  },
  resolve: {
    extensions: ['.js', '.html', '.scss', '.css']
  },
  devtool: 'source-map'
}
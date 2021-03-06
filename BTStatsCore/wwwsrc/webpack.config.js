var path = require('path');
var utils = require('./build/utils');
var HtmlWebpackPlugin = require('html-webpack-plugin');

const htmlPlugin = new HtmlWebpackPlugin({
  title: 'BT Stats',
  template: './src/index.html',
})

module.exports = {
  entry: {
    site: [
      './src/index.ts',
    ],
  },
  output: {
    filename: 'bundle.js',
    path: path.resolve(__dirname, '../wwwroot/'),
  },
  plugins: [htmlPlugin],
  module: {
    rules: [
      {
        test: /\.vue$/,
        loader: 'vue-loader',
        options: {
          loaders: utils.cssLoaders({
            sourceMap: false,
            extract: false
          })
        }
      },
      {
        test: /\.ts$/,
        exclude: /node_modules/,
        use: [
          { loader: 'babel-loader', options: { presets: ['env'] } },
          { loader: 'ts-loader', options: { appendTsSuffixTo: [/\.vue$/] } }
        ]
      },
      {
        test: /\.css$/,
        use: utils.cssLoaders().css
      },
      {
        test: /\.(ttf|eot|woff|woff2)$/,
        loader: "file-loader",
        options: {
          name: "fonts/[name].[ext]",
        },
      },
      {
        test: /\.(png|jp(e*)g|svg)$/,
        use: [{
          loader: 'url-loader',
          options: {
            limit: 8000, // Convert images < 8kb to base64 strings
            name: 'images/[hash]-[name].[ext]'
          }
        }]
      }
    ],
  },
  resolve: {
    extensions: ['.ts', '.vue', '.js'],
    alias: {
      'vue$': 'vue/dist/vue.esm.js'
    }
  },
  devtool: 'source-map',
  devServer: {
    contentBase: path.resolve(__dirname, '../wwwroot')
  }
}
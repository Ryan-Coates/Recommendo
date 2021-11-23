import Vue from 'vue'
import { BootstrapVue, IconsPlugin } from 'bootstrap-vue'
import 'bootstrap/dist/css/bootstrap.css'
import 'bootstrap-vue/dist/bootstrap-vue.css'
// import 'app.scss'
import VueAxios from './plugins/axios'
import App from './App.vue'
import './registerServiceWorker'
import router from './router'
import store from './store'

Vue.config.productionTip = false

// debug
// Vue.prototype.$apiEndpoint = 'https://localhost:5001/api/Recommendations'

// live
Vue.prototype.$apiEndpoint = 'https://rktv4mzdzt.eu-west-1.awsapprunner.com/api/Recommendations'

Vue.use(VueAxios)
Vue.use(BootstrapVue)
Vue.use(IconsPlugin)

new Vue({
  router,
  store,
  render: h => h(App)
}).$mount('#app')

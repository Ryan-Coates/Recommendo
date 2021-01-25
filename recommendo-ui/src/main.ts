import Vue from 'vue'
import VueAxios from './plugins/axios'
import App from './App.vue'
import './registerServiceWorker'
import router from './router'
import store from './store'

Vue.config.productionTip = false

// debug
// Vue.prototype.$apiEndpoint = 'https://localhost:5001/api/Recommendations'

// live
Vue.prototype.$apiEndpoint = 'https://recommendo-api.azurewebsites.net/api/Recommendations'

Vue.use(VueAxios)

new Vue({
  router,
  store,
  render: h => h(App)
}).$mount('#app')

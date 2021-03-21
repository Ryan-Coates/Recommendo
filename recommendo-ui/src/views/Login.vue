<template>
  <div>
  <v-facebook-login app-id='489770152036574'></v-facebook-login>
  <button v-on:click="doStuff()">Do stuff</button>  
  </div>
  
</template>

<script>
import VFacebookLogin from 'vue-facebook-login-component'

export default {
  components: {
    VFacebookLogin
  },
  methods: {
      doStuff () {
        if (VFacebookLogin.connected) {
          alert ('connected')
        }
      }
  }
}
</script>
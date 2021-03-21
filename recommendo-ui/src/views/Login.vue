<template>
  <v-facebook-login app-id='489770152036574'></v-facebook-login>
</template>

<script>
import VFacebookLogin from 'vue-facebook-login-component'

export default {
  components: {
    VFacebookLogin
  }
}

if (VFacebookLogin.connected) {
  alert ('connected')
}
</script>

<template>
  <div>
  <v-facebook-login app-id='489770152036574'></v-facebook-login>
  <button
    :style="inlineStyle"
    :disabled="scope.disabled"
  >
    <slot name="before" v-bind="scope"></slot>
    <slot name="loader" v-bind="scope">
      <v-loader
        :class="['loader', loaderClass]"
        v-if="scope.working"
        :style="loaderStyle"
      ></v-loader>
    </slot>
    <slot name="logo" v-bind="scope" v-if="scope.idle && scope.disconnected">
      <v-facebook-logo-alt
        :style="logoStyle"
        v-if="useAltLogo"
        :class="['logo', logoClass]"
      ></v-facebook-logo-alt>
      <v-facebook-logo
        v-else
        :style="logoStyle"
        :class="['logo', logoClass]"
      ></v-facebook-logo>
    </slot>
    <span :class="textClass" :style="textStyle">
      <slot name="login" v-bind="scope" v-if="scope.idle && scope.disconnected"
        >logged out</slot
      >
      <slot name="logout" v-bind="scope" v-if="scope.idle && scope.connected"
        >logged in</slot
      >
      <slot name="working" v-bind="scope" v-if="scope.working"
        >Please wait...</slot
      >
      <slot name="error" v-bind="scope" v-if="scope.hasError">⛔ Error</slot>
    </span>
    <slot name="after" v-bind="scope"></slot>
  </button>
  </div>
</template>

<script>
export default {
  components: {
  }
}
</script>

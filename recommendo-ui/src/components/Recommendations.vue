
<template>
    <div id="example">
        <h1>{{ type }}</h1>
        <p>{{ info }}</p>
    </div>
</template>
<script lang="ts">
import axios from 'axios'
import { Component, Prop, Vue } from 'vue-property-decorator'

export default {
  data () {
    return {
      info: ''
    }
  },
  created () {
    axios.get('https://api.coindesk.com/v1/bpi/currentprice.json')
      .then(response => { this.info = response.data })
  },
  props: {
    type: String
  }
}
</script>

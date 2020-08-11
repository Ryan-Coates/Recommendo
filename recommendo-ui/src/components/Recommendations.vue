
<template>
    <div>
        <h1>{{ type }}</h1>
        {{ info }}
    </div>
</template>
<script lang="ts">
import axios from 'axios'
import { Component, Prop, Vue } from 'vue-property-decorator'
@Component
export default class Recommendations extends Vue {
  @Prop() private type!: string;
}
data: {
    info: 'hello'
  }
axios.get('https://api.coindesk.com/v1/bpi/currentprice.json').then(response => (Vue.info = response))
</script>

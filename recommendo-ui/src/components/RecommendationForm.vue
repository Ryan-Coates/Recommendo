<template>
<div>
    <form v-on:submit.prevent="recommendationSubmit(name, description)" class="" action="#" method="post" :name=type data-netlify="true" data-netlify-honeypot="bot-field" >
    <input v-model="name" type="text" name="name" value="" placeholder="name">
    <input v-model="description" type="text" name="description" value="" placeholder="description">
    <button type="submit" name="button">Add {{type}}</button>
</form>
  <form
    name="ask-question"
    method="post"
    data-netlify="true"
    data-netlify-honeypot="bot-field"
    >
    <input type="hidden" name="form-name" value="ask-question" />
    <label v-for="(panelist, index) in panelists" :key="index">
      <input
        type="radio"
        name="panelist"
        :value="panelist"
        @input="ev => updatePanelist"
        :checked="panelist === currentPanelist"
      />
      <span>{{ panelist }}</span>
    </label>
    ...
    <button>Submit</button>
  </form>
  </div>
</template>
<script>
import axios from 'axios'
export default {
  name: 'RecommendationForm',
  props: {
    type: String
  },
  data () {
    return {
      name: '',
      description: '',
      panelists: ['Evan You', 'Chris Fritz'],
      currentPanelist: 'Evan You'
    }
  },
  methods: {
    recommendationSubmit (name, description) {
      this.$emit('addRecommendation', name, description)
      const axiosConfig = {
        header: { 'Content-Type': 'application/x-www-form-urlencoded' }
      }
      axios.post(
        '/',
        this.encode({
          'form-name': this.type,
          ...this.form
        }),
        axiosConfig)
    },
    encode (data) {
      return Object.keys(data)
        .map(
          key => `${encodeURIComponent(key)}=${encodeURIComponent(data[key])}`
        )
        .join('&')
    }

  }
}
</script>
<style>

</style>

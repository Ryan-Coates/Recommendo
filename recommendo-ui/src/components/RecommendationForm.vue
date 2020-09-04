<template>
<div>
  <h3>Feed me recommendations!!!!</h3>
    <form v-on:submit.prevent="recommendationSubmit(name, description)" class="" action="#" method="post" name="add-recommendation" data-netlify="true" data-netlify-honeypot="bot-field" >
    <input v-model="name" type="text" name="name" value="" placeholder="name">
    <input v-model="description" type="textarea" name="description" value="" placeholder="description">
    <br>
    <input v-model="recommender" type="text" name="recommender" value="" placeholder="what's your name?">
    <input v-model="category" type="text" name="category" value="" placeholder="category">
    <br>
    <button type="submit" name="button">Add {{type}}</button>
</form>
</div>
</template>
<script>
export default {
  name: 'RecommendationForm',
  props: {
    type: String
  },
  data () {
    return {
      name: '',
      description: '',
      category: this.type,
      recommender: ''
    }
  },
  methods: {
    recommendationSubmit (name, description, recommender) {
      this.$emit('addRecommendation', name, description, recommender)
      fetch('/', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: this.encode({
          'form-name': 'add-recommendation',
          name: this.name,
          description: this.description,
          category: this.category,
          recommender: this.recommender,
          ...this.form
        })
      })
        .then(() => {
          // this.$router.push('thanks')
        })
        .catch(() => {
          this.$router.push('404')
        })
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

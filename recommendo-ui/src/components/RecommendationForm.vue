<template>
<div>
<b-button v-b-toggle.collapse-1 variant="primary">Feed me recommendations v</b-button>
  <b-collapse id="collapse-1" class="mt-2">
    <b-card>
      <form v-on:submit.prevent="recommendationSubmit(name, description, recommender)" class="" action="#" method="post" name="add-recommendation" data-netlify="true" data-netlify-honeypot="bot-field" >
          <input v-model="name" type="text" name="name" value="" placeholder="name">
          <input v-model="description" type="textarea" name="description" value="" placeholder="description">
          <br>
          <input v-model="recommender" type="text" name="recommender" value="" placeholder="what's your name?">
          <input v-model="category" type="text" name="category" value="" placeholder="category">
          <br>
          <button type="submit" name="button">Add {{type}}</button>
        </form>
    </b-card>
  </b-collapse>
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
      const data = {
        name: name,
        description: description,
        type: this.type,
        recommender: recommender
      }
      fetch(this.$apiEndpoint, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
      })
        .then(() => {
          // this.$router.push('thanks')
        })
        .catch(() => {
          this.$router.push('404')
        })
    }
  }
}
</script>

<template>
    <form v-on:submit.prevent="recommendationSubmit(name, description)" class="" action="#" method="post">
    <input v-model="name" type="text" name="name" value="" placeholder="name">
    <input v-model="description" type="text" name="description" value="" placeholder="description">
    <button type="submit" name="button">Add {{type}}</button>
</form>
</template>
<script>
export default {
  name: 'RecommendationForm',
  props: [
    'recommendation'
  ],
  data () {
    return {
      name: '',
      description: ''
    }
  },
  methods: {
    recommendationSubmit (name, description) {
      this.$emit('addRecommendation', name, description)
    }

  }
}
</script>
<style>

</style>

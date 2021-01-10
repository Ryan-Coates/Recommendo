<template>
  <div class="movies">
    <Recommendations type='movies' :recommendations=recommendations />
  </div>
</template>

<script>
import Recommendations from '@/components/Recommendations.vue'
export default {
  name: 'movies',
  components: {
    Recommendations
  },
  data () {
    return {
      name: 'movies',
      recommendations: []
    }
  },
  mounted () {
    fetch(this.$apiEndpoint + '/?type=' + this.name).then(response => response.json()).then(data => { this.recommendations = data })
  }
}
</script>

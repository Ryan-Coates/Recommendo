<template>
  <div class="games">
    <Recommendations type='games' :recommendations=recommendations />
  </div>
</template>

<script>
import Recommendations from '@/components/Recommendations.vue'
export default {
  name: 'games',
  components: {
    Recommendations
  },
  data () {
    return {
      name: 'games',
      recommendations: []
    }
  },
  mounted () {
    fetch(this.$apiEndpoint + '/?type=' + this.name).then(response => response.json()).then(data => { this.recommendations = data })
  }
}
</script>

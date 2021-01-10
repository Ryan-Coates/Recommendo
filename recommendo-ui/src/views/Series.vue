<template>
  <div class="series">
    <Recommendations type='series' :recommendations=recommendations />
  </div>
</template>

<script>
import Recommendations from '@/components/Recommendations.vue'
export default {
  name: 'series',
  components: {
    Recommendations
  },
  data () {
    return {
      name: 'series',
      recommendations: []
    }
  },
  mounted () {
    fetch(this.$apiEndpoint + '/?type=' + this.name).then(response => response.json()).then(data => { this.recommendations = data })
  }
}
</script>

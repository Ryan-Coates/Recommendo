<template>
  <div class="daysout">
    <Recommendations type='days out' :recommendations=recommendations />
  </div>
</template>

<script>
import Recommendations from '@/components/Recommendations.vue'
export default {
  name: 'daysout',
  components: {
    Recommendations
  },
  data () {
    return {
      name: 'daysout',
      recommendations: []
    }
  },
  mounted () {
    fetch(this.$apiEndpoint + '/?type=' + this.name).then(response => response.json()).then(data => { this.recommendations = data })
  }
}
</script>

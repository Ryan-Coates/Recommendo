<template>
  <div class="kidsactivities">
    <Recommendations type='kidsactivities' :recommendations=recommendations />
  </div>
</template>

<script>
import Recommendations from '@/components/Recommendations.vue'
export default {
  name: 'kidsactivities',
  components: {
    Recommendations
  },
  data () {
    return {
      name: 'kidsactivities',
      recommendations: []
    }
  },
  mounted () {
    fetch(this.$apiEndpoint + '/?type=' + this.name).then(response => response.json()).then(data => { this.recommendations = data })
  }
}
</script>

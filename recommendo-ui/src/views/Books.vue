<template>
  <div class="books">
    <Recommendations type='books' :recommendations=recommendations />
  </div>
</template>

<script>
import Recommendations from '@/components/Recommendations.vue'
export default {
  name: 'books',
  components: {
    Recommendations
  },
  data () {
    return {
      name: 'books',
      recommendations: []
    }
  },
  mounted () {
    fetch(this.$apiEndpoint + '/?type=' + this.name).then(response => response.json()).then(data => { this.recommendations = data })
  }
}
</script>

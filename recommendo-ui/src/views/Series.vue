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
      recommendations: [
        { name: 'Series Placeholder', description: 'Add recommendations so i can go away!', recommender: 'Ryan' }
      ]
    }
  }
}
</script>

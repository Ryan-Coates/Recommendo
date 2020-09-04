<template>
  <div class="restaurants">
    <Recommendations type='restaurants' :recommendations=recommendations />
  </div>
</template>

<script>
import Recommendations from '@/components/Recommendations.vue'
export default {
  name: 'restaurants',
  components: {
    Recommendations
  },
  data () {
    return {
      recommendations: [
        { name: 'Restaurants Placeholder', description: 'Add recommendations so i can go away!', recommender: 'Ryan' }
      ]
    }
  }
}
</script>

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
      recommendations: [
        { name: 'Games Placeholder', description: 'Add recommendations so i can go away!', recommender: 'Ryan' }
      ]
    }
  }
}
</script>

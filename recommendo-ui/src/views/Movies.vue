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
      recommendations: [
        { name: 'Movies Placeholder', description: 'Add recommendations so i can go away!', recommender: 'Ryan' }
      ]
    }
  }
}
</script>

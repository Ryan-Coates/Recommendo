<template>
  <div class="music">
    <Recommendations type='music' :recommendations=recommendations />
  </div>
</template>

<script>
import Recommendations from '@/components/Recommendations.vue'
export default {
  name: 'music',
  components: {
    Recommendations
  },
  data () {
    return {
      recommendations: [
        { name: 'Music Placeholder', description: 'Add recommendations so i can go away!', recommender: 'Ryan' }
      ]
    }
  }
}
</script>

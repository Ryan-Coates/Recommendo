<template>
  <div class="recipes">
    <Recommendations type='recipes' :recommendations=recommendations />
  </div>
</template>

<script>
import Recommendations from '@/components/Recommendations.vue'
export default {
  name: 'recipes',
  components: {
    Recommendations
  },
  data () {
    return {
      recommendations: [
        { name: 'Recipes Placeholder', description: 'Add recommendations so i can go away!', recommender: 'Ryan' }
      ]
    }
  }
}
</script>

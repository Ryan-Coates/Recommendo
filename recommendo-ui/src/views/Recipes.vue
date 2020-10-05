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
      recommendations: []
    }
  },
  mounted () {
    fetch('https://recommendo-api.knisuel.com//api/recipes').then(response => response.json()).then(data => { this.recommendations = data })
  }
}
</script>

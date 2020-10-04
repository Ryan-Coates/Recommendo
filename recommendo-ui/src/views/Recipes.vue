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
  methods: {
    fetchData () {
      fetch('https://recomendo-api.kenisuel.com/api/recipes')
        .then(response => response.json())
        .then(data => console.log(data))
    }
  },
  data () {
    return {
      recommendations: this.fetchData()
    }
  }
}
</script>

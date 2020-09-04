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
      recommendations: [
        { name: 'Books Placeholder', description: 'Add recommendations so i can go away!', recommender: 'Ryan' }
      ]
    }
  }
}
</script>

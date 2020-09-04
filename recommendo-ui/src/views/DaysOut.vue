<template>
  <div class="daysout">
    <Recommendations type='days out' :recommendations=recommendations />
  </div>
</template>

<script>
import Recommendations from '@/components/Recommendations.vue'
export default {
  name: 'daysout',
  components: {
    Recommendations
  },
  data () {
    return {
      recommendations: [
        { name: 'Days Out Placeholder', description: 'Add recommendations so i can go away!', recommender: 'Ryan' }
      ]
    }
  }
}
</script>

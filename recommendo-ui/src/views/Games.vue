<template>
  <div class="games">
    <Recommendations type='games'/>
  </div>
</template>

<script>
import Recommendations from '@/components/Recommendations.vue'
export default {
  name: 'games',
  components: {
    Recommendations
  }
}
</script>

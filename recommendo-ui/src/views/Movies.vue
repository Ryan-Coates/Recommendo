<template>
  <div class="movies">
    <Recommendations type='movies'/>
  </div>
</template>

<script>
import Recommendations from '@/components/Recommendations.vue'
export default {
  name: 'movies',
  components: {
    Recommendations
  }
}
</script>

<template>
  <div class="music">
    <Recommendations type='music'/>
  </div>
</template>

<script>
import Recommendations from '@/components/Recommendations.vue'
export default {
  name: 'music',
  components: {
    Recommendations
  }
}
</script>

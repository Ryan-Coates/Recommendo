<template>
  <div class="restaurants">
    <Recommendations type='restaurants'/>
  </div>
</template>

<script>
import Recommendations from '@/components/Recommendations.vue'
export default {
  name: 'restaurants',
  components: {
    Recommendations
  }
}
</script>

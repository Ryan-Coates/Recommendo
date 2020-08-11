<template>
  <div class="recipes">
    <Recommendations type='recipes'/>
  </div>
</template>

<script>
import Recommendations from '@/components/Recommendations.vue'
export default {
  name: 'recipes',
  components: {
    Recommendations
  }
}
</script>

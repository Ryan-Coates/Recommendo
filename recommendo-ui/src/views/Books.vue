<template>
  <div class="books">
    <Recommendations type='books'/>
  </div>
</template>

<script>
import Recommendations from '@/components/Recommendations.vue'
export default {
  name: 'books',
  components: {
    Recommendations
  }
}
</script>

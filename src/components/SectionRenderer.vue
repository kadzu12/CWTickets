<template>
    <div class="hall-container">
      <div 
        class="seat-tooltip" 
        v-if="tooltip.visible"
        :style="{ left: `${tooltip.x + 10}px`, top: `${tooltip.y + 10}px` }"
      >
        <div>{{ tooltip.content }}</div>
        <div>{{ tooltip.price }} ₽</div>
      </div>
      
      <svg 
      class="hall-svg"
      :viewBox="`0 0 ${viewBoxWidth} ${viewBoxHeight}`"
      preserveAspectRatio="xMidYMid meet"
    >
      <g v-for="section in sections" :key="section.idSection">
        <path 
          :d="section.schemaSection"
          class="section-path"
          :fill="getSectionColor(section.priceSection)"
          stroke="#fff"
          stroke-width="2"
        />

        <text
          :x="getTextPosition(section.schemaSection).x"
          :y="getTextPosition(section.schemaSection).y"
          text-anchor="middle"
          font-size="30"
          fill="white"
          font-weight="bold"
        >
          {{ section.nameSection }}
        </text>

<template v-if="section.typeSection === 'Столы'">
          <g v-for="(table, index) in generateTables(section) || []" :key="'table-'+index">
            <circle
              :cx="table.center.x"
              :cy="table.center.y"
              :r="tableRadius"
              fill="white"
              stroke="#333"
              stroke-width="2"
            />
            
            <circle
              v-for="(chair, cIndex) in table.chairs"
              :key="'chair-'+index+'-'+cIndex"
              :cx="chair.x"
              :cy="chair.y"
              :r="chairRadius"
              :fill="getChairColor(table, cIndex+1)"
              stroke="#333"
              stroke-width="1"
              @mouseenter="showSeatTooltip($event, { 
                sectionType: 'Столы', 
                tableNumber: table.tableNumber, 
                chairNumber: cIndex+1, 
                price: table.price 
              })"
              @mouseleave="hideSeatTooltip"
              :class="{ 'unavailable-seat': !isChairAvailable(table, cIndex+1) }"
              @click="isChairAvailable(table, cIndex+1) && toggleChairSelection(table, cIndex+1)"
            />
          </g>
        </template>

        <template v-else-if="section.typeSection === 'Ряды'">
          <g v-for="(row, rowIndex) in generateRows(section)" :key="'row-'+rowIndex">
            <rect
              v-for="(seat, seatIndex) in row.seats"
              :key="'seat-'+rowIndex+'-'+seatIndex"
              :x="seat.x"
              :y="seat.y"
              width="20"
              height="20"
              rx="4"
              :fill="isSeatSelected(seat) ? '#8a2be2' : '#999'"
              stroke="#333"
              stroke-width="1"
              @mouseenter="showSeatTooltip($event, { 
                sectionType: 'Ряды', 
                rowNumber: seat.rowNumber, 
                chairNumber: seat.chairNumber, 
                price: seat.price 
              })"
              @mouseleave="hideSeatTooltip"
              :class="{ 'unavailable-seat': seat.isAvailable === false }"
              @click="seat.isAvailable !== false && toggleSeatSelection(seat)"
            />
          </g>
        </template>
  
          <template v-else-if="section.typeSection === 'Танцпол'">
            <rect
              :x="getDancefloorPosition(section).x"
              :y="getDancefloorPosition(section).y"
              width="100"
              height="60"
              rx="10"
              :fill="dancefloorSelected ? '#8a2be2' : (isDancefloorAvailable(section) ? '#999' : '#ccc')"
              @mouseenter="showSeatTooltip($event, { 
                sectionType: 'Танцпол', 
                price: section.priceSection 
              })"
              @mouseleave="hideSeatTooltip"
              @click="isDancefloorAvailable(section) && selectDancefloor(section)"
              style="cursor: pointer;"
            />
            <text
              :x="getDancefloorPosition(section).x + 50"
              :y="getDancefloorPosition(section).y + 35"
              text-anchor="middle"
              fill="white"
              font-weight="bold"
            >
              Танцпол
            </text>
          </template>

        </g>
      </svg>
    </div>
  </template>
  
  <script setup>
  import { ref, computed, onMounted } from 'vue'
  import ApiService from '@/assets/services/apiService';
  const props = defineProps({
    sections: Array,
    selectedSeats: Array,
    concert: [Object, String, Number]
  })
  const getDancefloorAvailability = (section) => {
  const total = section.totalSeatsSection || 0;
  const taken = occupiedSeats.value.filter(
    s => s.sectionId === section.idSection
  ).length;
  return total - taken;
};

const isDancefloorAvailable = (section) => {
  return getDancefloorAvailability(section) > 0;
};

  const emit = defineEmits(['select-section', 'toggle-seat', 'select-dancefloor'])

  const isPointInPath = (pathData, x, y) => {
    const svgNS = "http://www.w3.org/2000/svg"
    const svg = document.createElementNS(svgNS, "svg")
    const path = document.createElementNS(svgNS, "path")
    path.setAttribute("d", pathData)
    svg.appendChild(path)
    document.body.appendChild(svg)
    const point = svg.createSVGPoint()
    point.x = x
    point.y = y
    const result = path.isPointInFill(point)
    document.body.removeChild(svg)
    return result
  }
  
  const tableRadius = 15
  const chairRadius = 8
  const viewBoxWidth = ref(1000)
  const viewBoxHeight = ref(1000)
  
  const tooltip = ref({
    visible: false,
    x: 0,
    y: 0,
    content: '',
    price: 0
  })
  const isChairAvailable = (table, chairNumber) => {
  const seat = table.chairs[chairNumber - 1];
  return seat?.isAvailable !== false;
    };

    const getChairColor = (table, chairNumber) => {
    const selected = isChairSelected(table, chairNumber);
    const seat = table.chairs[chairNumber - 1];
    if (!seat) return '#999';
    if (selected) return '#8a2be2';
    return seat.isAvailable === false ? '#ccc' : '#999';
    };

  const dancefloorSelected = computed(() => {
    return props.selectedSeats?.some(seat => seat.isDancefloor)
  })
  const isChairSelected = (table, chairNumber) => {
  return props.selectedSeats?.some(seat => 
    seat.isTable &&
    seat.section.idSection === table.sectionId &&
    seat.unitNumber === table.tableNumber &&
    seat.chairNumber === chairNumber
  );
};

const isSeatSelected = (seat) => {
  return props.selectedSeats?.some(s => 
    !s.isTable &&
    s.section.idSection === seat.sectionId &&
    s.unitNumber === seat.rowNumber &&
    s.chairNumber === seat.chairNumber
  );
};
  
  const showSeatTooltip = (event, seat) => {
  let content = ''
  if (seat.sectionType === 'Столы') {
    content = `Стол ${seat.tableNumber}, Место ${seat.chairNumber}`
  } else if (seat.sectionType === 'Ряды') {
    content = `Ряд ${seat.rowNumber}, Место ${seat.chairNumber}`
  } else if (seat.sectionType === 'Танцпол') {
    const section = props.sections.find(s => s.typeSection === 'Танцпол');
    const available = getDancefloorAvailability(section);
    content = `Танцпол, \nсвободно: ${available}`;
  }

  tooltip.value = {
    visible: true,
    x: event.clientX,
    y: event.clientY,
    content: content,
    price: seat.price
  }
}

  
  const hideSeatTooltip = () => {
    tooltip.value.visible = false
  }
  
  const toggleChairSelection = (table, chairNumber) => {
  const section = props.sections.find(s => s.idSection === table.sectionId);
  if (!section) return;
  
  emit('toggle-seat', {
    section: section, 
    unitNumber: table.tableNumber,
    chairNumber: chairNumber,
    price: table.price,
    isTable: true
  });
}

const toggleSeatSelection = (seat) => {
  const section = props.sections.find(s => s.idSection === seat.sectionId);
  if (!section) return;
  
  emit('toggle-seat', {
    section: section,
    unitNumber: seat.rowNumber,
    chairNumber: seat.chairNumber,
    price: seat.price,
    isTable: false
  });
}
  
  const selectDancefloor = (section) => {
  emit('select-dancefloor', section);
};
  
  const getDancefloorPosition = (section) => {
    const bounds = parsePathBounds(section.schemaSection)
    return {
      x: bounds ? bounds.minX + bounds.width/2 - 50 : 100,
      y: bounds ? bounds.minY + bounds.height/2 - 30 : 100
    }
  }
  
  const priceColors = ['#ff9999', '#ff6666', '#ff3333', '#ff0000', '#cc0000']
  
  const getSectionColor = (price) => {
    const minPrice = Math.min(...props.sections.map(s => s.priceSection))
    const maxPrice = Math.max(...props.sections.map(s => s.priceSection))
    
    if (maxPrice === minPrice) return priceColors[0]
    
    const ratio = (price - minPrice) / (maxPrice - minPrice)
    const index = Math.min(Math.floor(ratio * priceColors.length), priceColors.length - 1)
    return priceColors[index]
  }

  const generateRows = (section) => {
    const rows = [];
    const rowCount = section.unitCountSection || 10;
    const seatsPerRow = section.seatsPerUnitSection || 10;

    const bounds = parsePathBounds(section.schemaSection);
    if (!bounds) return rows;

    const rowHeight = bounds.height / (rowCount + 2);
    const seatWidth = bounds.width / (seatsPerRow + 2);

    for (let row = 0; row < rowCount; row++) {
      const rowSeats = [];
      const rowY = bounds.minY + 25 + (row + 1) * rowHeight;

      for (let seat = 0; seat < seatsPerRow; seat++) {
        const seatX = bounds.minX + (seat + 1) * seatWidth;
        
        if (!isPointInPath(section.schemaSection, seatX, rowY)) continue;
        
        const isTaken = occupiedSeats.value.some(t =>
          t.sectionId === section.idSection &&
          t.row === row + 1 &&
          t.seat === seat + 1
        );
        const isAvailable = !isTaken;

        rowSeats.push({
          x: seatX - 10, 
          y: rowY - 10,
          rowNumber: row + 1,
          chairNumber: seat + 1,
          sectionId: section.idSection,
          sectionType: section.typeSection,
          price: section.priceSection,
          isAvailable
        });
      }
      
      if (rowSeats.length > 0) {
        rows.push({
          rowNumber: row + 1,
          seats: rowSeats
        });
      }
    }

    return rows;
  };
  
const generateTables = (section) => {
  const tables = [];
  const tableCount = section.unitCountSection || 20;
  const chairsPerTable = section.seatsPerUnitSection || 5;

  const bounds = parsePathBounds(section.schemaSection);
  if (!bounds) return tables;

  const maxCols = Math.ceil(Math.sqrt(tableCount * bounds.width / bounds.height));
  const maxRows = Math.ceil(tableCount / maxCols);

  let dynamicTableRadius = tableRadius;
  let dynamicChairRadius = chairRadius;

  let scale = 1;
  while (scale > 0.5) {
    const spacingX = bounds.width / (maxCols + 1);
    const spacingY = bounds.height / (maxRows + 1);
    const requiredSpace = (dynamicTableRadius + dynamicChairRadius + 4) * 2;

    if (requiredSpace < spacingX && requiredSpace < spacingY) break;

    scale -= 0.05;
    dynamicTableRadius = tableRadius * scale;
    dynamicChairRadius = chairRadius * scale;
  }

  const cols = maxCols;
  const rows = Math.ceil(tableCount / cols);
  const spacingX = bounds.width / (cols + 1);
  const spacingY = bounds.height / (rows + 1);

  const baseAngles = Array.from({ length: chairsPerTable }, (_, i) =>
    (Math.PI * 2 * i) / chairsPerTable
  );

  for (let row = 0; row < rows; row++) {
    for (let col = 0; col < cols; col++) {
      if (tables.length >= tableCount) break;

      const centerX = bounds.minX + (col + 1) * spacingX;
      const centerY = bounds.minY + (row + 1) * spacingY;

      if (!isPointInPath(section.schemaSection, centerX, centerY)) continue;

      const center = {
        x: centerX,
        y: centerY,
        sectionId: section.idSection,
        tableNumber: tables.length + 1
      };

      const chairs = baseAngles.map((angle, i) => {
        const x = center.x + (dynamicTableRadius + dynamicChairRadius + 4) * Math.cos(angle);
        const y = center.y + (dynamicTableRadius + dynamicChairRadius + 4) * Math.sin(angle);
        const isAvailable = !occupiedSeats.value.some(
          t =>
            t.sectionId === section.idSection &&
            t.row === center.tableNumber &&
            t.seat === i + 1
        );
        return { x, y, isAvailable };
      });

      tables.push({
        center,
        chairs,
        tableNumber: center.tableNumber,
        sectionId: section.idSection,
        sectionType: section.typeSection,
        price: section.priceSection
      });
    }
  }

  return tables;
};

  const parsePathBounds = (pathData) => {
    if (!pathData) return null
    
    const points = pathData.match(/[ML]\s*([\d.]+)\s*([\d.]+)/g)
    if (!points || points.length === 0) return null
    
    let minX = Infinity, minY = Infinity, maxX = -Infinity, maxY = -Infinity
    
    points.forEach(point => {
      const coords = point.match(/[ML]\s*([\d.]+)\s*([\d.]+)/)
      if (coords) {
        const x = parseFloat(coords[1])
        const y = parseFloat(coords[2])
        minX = Math.min(minX, x)
        minY = Math.min(minY, y)
        maxX = Math.max(maxX, x)
        maxY = Math.max(maxY, y)
      }
    })
    
    return {
      minX, minY, maxX, maxY,
      width: maxX - minX,
      height: maxY - minY
    }
  }
  
  const getTextPosition = (pathData) => {
    const bounds = parsePathBounds(pathData)
    return {
      x: bounds ? bounds.minX + bounds.width / 2 : 100,
      y: bounds ? bounds.minY + 40 : 100
    }
  }
  const occupiedSeats = ref([]);

const loadOccupiedSeats = async () => {
  try {
    const data = await ApiService.getOccupiedSeats(props.concert);
    occupiedSeats.value = data;
  } catch (error) {
    console.error('Ошибка загрузки занятых мест:', error);
  }
};
  
  onMounted(async() => {
    let minX = Infinity, minY = Infinity, maxX = -Infinity, maxY = -Infinity
  props.sections.forEach(section => {
    const bounds = parsePathBounds(section.schemaSection)
    if (bounds) {
      minX = Math.min(minX, bounds.minX)
      minY = Math.min(minY, bounds.minY)
      maxX = Math.max(maxX, bounds.maxX)
      maxY = Math.max(maxY, bounds.maxY)
    }
  })
  if (minX !== Infinity) {
    viewBoxWidth.value = maxX
    viewBoxHeight.value = maxY
  }
  await loadOccupiedSeats();
  })
  </script>
  
  <style scoped>
  .hall-container {
    position: relative;
    width: auto;
    height: 50%;
    min-height: 600px;
    border-radius: 8px;
    overflow: hidden;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  }
  
  .hall-svg {
    width: 100%;
    height: 100%;
    padding-top: 100px;
    padding-bottom: -100px;
    padding-left: 30%;
    background-color: #f8f8f8;
  }
  
  .section-path {
    opacity: 0.8;
    transition: opacity 0.3s;
  }
  
  .section-path:hover {
    opacity: 1;
  }
  
  .seat-tooltip {
    position: fixed;
    background: rgba(0, 0, 0, 0.8);
    color: white;
    padding: 8px 12px;
    border-radius: 4px;
    font-size: 14px;
    pointer-events: none;
    z-index: 1000;
    transform: translate(10px, 10px);
  }
  .unavailable-seat {
  pointer-events: none;
  opacity: 0.4;
}
  </style>